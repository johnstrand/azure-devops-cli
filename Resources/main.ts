import { get } from "node:https";
import {
  createWriteStream,
  existsSync,
  unlinkSync,
  mkdirSync,
  readdirSync,
  statSync,
  readFileSync,
  writeFileSync,
} from "node:fs";
import { open } from "yauzl";
import { dirname, join } from "node:path";
import type { OpenAPI, Parameter } from "./schema";

/**
 * Downloads a file from the specified URL
 * @param url URL to download
 * @returns Promise that resolves when the download is complete
 */
function downloadFileAsync(url: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const stream = createWriteStream("./spec.zip");

    get(url, (res) => {
      res.pipe(stream);
      res.on("error", (err) => {
        // If an error occurs, delete the file and reject the promise
        unlinkSync("./spec.zip");
        console.error(err);
        reject(err);
      });

      stream.on("finish", () => {
        // All done, close up shop
        stream.close();
        resolve();
      });
    }).on("error", (err) => {
      // If an error occurs, delete the file and reject the promise
      unlinkSync("./spec.zip");
      console.error(err);
      reject(err);
    });
  });
}

/**
 * Unzips the source archive
 * @returns Promise that resolves when the extraction is complete
 */
function unzipAsync() {
  return new Promise<void>((resolve) => {
    open("./spec.zip", { lazyEntries: true }, (err, zipfile) => {
      // Move to the first entry
      zipfile.readEntry();

      // When we have an entry to process
      zipfile.on("entry", (entry) => {
        // If entry name ends with a slash, it's a directory
        if (/\/$/.test(entry.fileName)) {
          // If the directory doesn't exist, create it
          if (!existsSync(entry.fileName)) {
            mkdirSync(entry.fileName);
          }
          // Move to the next entry
          zipfile.readEntry();
        } else {
          zipfile.openReadStream(entry, (err, readStream) => {
            if (err) throw err;
            // If the file's directory doesn't end with 7.1, skip it
            if (!dirname(entry.fileName).endsWith("7.1")) {
              // But still move to the next entry
              zipfile.readEntry();
              return;
            }
            console.log(entry.fileName);

            readStream.on("end", () => {
              zipfile.readEntry();
            });

            readStream.pipe(createWriteStream(entry.fileName));
          });
        }
      });
      zipfile.on("end", resolve);
    });
  });
}

/**
 * Recursively enumerates all files in the specified directory
 * @param path The path to enumerate
 * @returns An array of paths to all files in the specified directory
 */
function enumerateSpecifications(path: string) {
  const entries: string[] = [];
  for (const entry of readdirSync(path)) {
    const fullPath = join(path, entry);
    if (statSync(fullPath).isDirectory()) {
      entries.push(...enumerateSpecifications(fullPath));
    } else {
      entries.push(fullPath);
    }
  }

  return entries;
}

console.log("Running model generation...");

console.log("Checking for source archive");

// If the source archive doesn't exist, download it
if (!existsSync("./spec.zip")) {
  console.log("Downloading source archive");
  await downloadFileAsync(
    "https://codeload.github.com/MicrosoftDocs/vsts-rest-api-specs/zip/refs/heads/master"
  );
  console.log("Download complete");
} else {
  console.log("Source archive found");
}

// If the source directory doesn't exist, extract the archive
if (!existsSync("./vsts-rest-api-specs-master")) {
  console.log("Extracting source archive");
  await unzipAsync();
  console.log("Extraction complete");
}

console.log("Generating model");
const model: Record<string, any> = {};

// TODO: Generate C# types for all exported types
const exportedTypes = new Set<string>();

/**
 * Generate a model for the specified parameter type
 * @param type Parameter type, which can be path, query, header, or body
 * @param parameters The parameters to generate the model from
 * @returns An array of parameter models, or undefined if the parameter list is undefined
 */
function parameterModel(
  type: "path" | "query" | "header" | "body",
  parameters: Parameter[] | undefined
) {
  return parameters
    ?.filter((p) => p.in === type)
    .map((p) => ({
      name: p.name,
      description: p.description,
      required: p.required,
      type: p.type,
      format: p.format,
      schema: p.schema,
      enum: p["x-ms-enum"],
    }));
}

function createModel(api: OpenAPI) {
  const host = api.host;
  const area = api["x-ms-vss-area"].toLowerCase();

  console.log(`Exporting ${area}`);

  if (!model[area]) {
    model[area] = {};
  }

  for (const [urlTemplate, operations] of Object.entries(api.paths).concat(
    Object.entries(api["x-ms-paths"] ?? {})
  )) {
    for (const [verb, operation] of Object.entries(operations)) {
      const name = operation["x-ms-vss-method"];
      console.log(`\t${name}`);

      if (!model[area][name]) {
        model[area][name] = [];
      }

      model[area][name].push({
        urlTemplate,
        verb,
        apiVersion: operation["x-ms-docs-override-version"],
        description: operation.description,
        operationId: operation.operationId,
        host,
        parameters: {
          path: parameterModel("path", operation.parameters),
          query: parameterModel("query", operation.parameters),
          header: parameterModel("header", operation.parameters),
          body: parameterModel("body", operation.parameters)?.[0],
        },
        responses: operation.responses,
      });

      for (const definition of Object.values(operation.responses)) {
        if (!definition.schema) {
          continue;
        }

        const name = definition.schema["$ref"];

        // If the definition has a name, add it to the list of exported types
        if (name) {
          exportedTypes.add(name);
        } else if (definition.schema.items && definition.schema.items["$ref"]) {
          // If the definition is an array, add the item type to the list of exported types
          exportedTypes.add(definition.schema.items["$ref"]);
        } else if (
          definition.schema.additionalProperties &&
          definition.schema.additionalProperties["$ref"] // If the definition inherits from a base type, add the base type to the list of exported types
        ) {
          exportedTypes.add(definition.schema.additionalProperties["$ref"]);
        }
      }
    }
  }
}

for (const specFile of enumerateSpecifications(
  "./vsts-rest-api-specs-master"
)) {
  const spec = JSON.parse(
    readFileSync(specFile, "utf8").substring(1)
  ) as OpenAPI;

  createModel(spec);
}

const output = JSON.stringify(model, null, 2);

writeFileSync("../operations.json", output, "utf8");

console.log("Model generation complete");
