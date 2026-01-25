#!/usr/bin/env node

/**
 * Post-compilation script to add .js extensions to relative imports in ESM output.
 * Node.js ESM requires explicit file extensions for relative imports.
 *
 * This script processes all .js files in the bin/ directory and adds .js extensions
 * to relative import/export statements that don't already have them.
 */

import { readdir, readFile, writeFile, stat } from "node:fs/promises";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const binDir = join(__dirname, "..", "bin");

// Regex to match relative imports/exports without file extensions
// Matches: from "./path" or from "../path" (not from "package")
const importExportRegex = /(from\s+["'])(\.\.?\/[^"']+)(["'])/g;

/**
 * Check if a path already has a file extension
 */
function hasExtension(path) {
  const lastSegment = path.split("/").pop();
  return lastSegment.includes(".");
}

/**
 * Add .js extension to import path if it doesn't have one
 */
function addJsExtension(match, prefix, path, suffix) {
  if (hasExtension(path)) {
    return match; // Already has extension
  }
  return `${prefix}${path}.js${suffix}`;
}

/**
 * Process a single .js file
 */
async function processFile(filePath) {
  const content = await readFile(filePath, "utf-8");
  const newContent = content.replace(importExportRegex, addJsExtension);

  if (content !== newContent) {
    await writeFile(filePath, newContent, "utf-8");
  }
}

/**
 * Recursively process all .js files in a directory
 */
async function processDirectory(dir) {
  const entries = await readdir(dir);

  for (const entry of entries) {
    const fullPath = join(dir, entry);
    const stats = await stat(fullPath);

    if (stats.isDirectory()) {
      await processDirectory(fullPath);
    } else if (entry.endsWith(".js")) {
      await processFile(fullPath);
    }
  }
}

try {
  await processDirectory(binDir);
} catch (error) {
  if (error.code === "ENOENT") {
    console.error(
      "Error: bin/ directory not found. Run tsc first to compile TypeScript.",
    );
    process.exit(1);
  }
  throw error;
}
