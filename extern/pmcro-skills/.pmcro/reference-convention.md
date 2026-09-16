# Writing a reference doc (external system, not local implementation)

Any file in this repo that describes another framework or product's design — Microsoft Agent
Framework, the Agent Skills spec, a Clean Architecture variant, anything not implemented here —
follows one shape, so a reader never confuses "describes X" with "pmcro does X":

1. **Open with a ground-truth notice** stating this describes an external system and does not by
   itself establish that pmcro implements it.
2. **Body**: the actual mapping — what the external concept is, and (where relevant) which pmcro
   concept it corresponds to.
3. **Close with a `Sources:` line** (or `## Resources` section) listing the exact URLs the content
   was drawn from, so a claim can be checked rather than taken on faith.

`.pmcro/interception-points.md` and `.pmcro/orchestration.md` already follow this shape — use them
as the template rather than reinventing the format per file.

This is deliberately not its own plugin, skill, or `references/` subsystem (an older, since-rejected
design proposed a whole `pmcro-references` plugin with per-vendor folders, scripts, and asset
schemas for this). A convention documented once here is enough; it doesn't need runtime code.
