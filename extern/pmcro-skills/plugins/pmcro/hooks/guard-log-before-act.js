#!/usr/bin/env node
'use strict';

/**
 * Enforces EC-SYS-003 (Log Before Act): a Write or Edit to any path outside
 * .pmcro/ is denied unless a trail is currently open under .pmcro/trails/
 * (a directory holding 00-frame.jsonl but no disposition.json yet).
 *
 * Reads a PreToolUse payload from stdin per
 * https://code.claude.com/docs/en/hooks and communicates its decision via
 * exit 0 + JSON on stdout (the documented, recommended mechanism), never via
 * exit code 2 — a malformed or unreadable payload must fail open, not crash
 * or silently block unrelated work.
 */

const fs = require('fs');
const path = require('path');

function emit(decision) {
  if (decision) {
    process.stdout.write(JSON.stringify({
      hookSpecificOutput: {
        hookEventName: 'PreToolUse',
        permissionDecision: decision.permissionDecision,
        permissionDecisionReason: decision.permissionDecisionReason,
      },
    }));
  }
  process.exit(0);
}

function trailIsOpen(dir) {
  const frame = path.join(dir, '00-frame.jsonl');
  const disposition = path.join(dir, 'disposition.json');
  return fs.existsSync(frame) && !fs.existsSync(disposition);
}

function hasAnyOpenTrail(trailsDir) {
  let found = false;
  (function walk(dir) {
    if (found) return;
    let entries;
    try {
      entries = fs.readdirSync(dir, { withFileTypes: true });
    } catch {
      return;
    }
    for (const entry of entries) {
      if (!entry.isDirectory()) continue;
      const full = path.join(dir, entry.name);
      if (trailIsOpen(full)) {
        found = true;
        return;
      }
      walk(full);
      if (found) return;
    }
  })(trailsDir);
  return found;
}

let raw = '';
process.stdin.setEncoding('utf8');
process.stdin.on('data', (chunk) => { raw += chunk; });
process.stdin.on('error', () => emit(null));
process.stdin.on('end', () => {
  let payload;
  try {
    payload = JSON.parse(raw);
  } catch {
    emit(null);
    return;
  }

  const toolInput = payload.tool_input || {};
  const filePath = toolInput.file_path;
  if (!filePath) {
    emit(null);
    return;
  }

  const cwd = payload.cwd || process.cwd();
  const abs = path.isAbsolute(filePath) ? filePath : path.resolve(cwd, filePath);
  const rel = path.relative(cwd, abs);

  if (rel.split(path.sep)[0] === '.pmcro') {
    emit(null);
    return;
  }

  const trailsDir = path.join(cwd, '.pmcro', 'trails');
  if (hasAnyOpenTrail(trailsDir)) {
    emit(null);
    return;
  }

  emit({
    permissionDecision: 'deny',
    permissionDecisionReason:
      'EC-SYS-003 (Log Before Act): no open trail under .pmcro/trails/. ' +
      'Open a trail frame (.pmcro/trails/{uuid}/00-frame.jsonl) before mutating "' + rel + '".',
  });
});
