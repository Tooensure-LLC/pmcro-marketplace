<#
.SYNOPSIS
    Emits DsoCatalog.json from the real pmcro-skills tree.

.DESCRIPTION
    Every PMCR-O resource is a DSO: a bounded, reusable, domain-specific object.
    A plugin, a skill, a reference document, an asset, an agent definition, a hook -
    each is the same shape, so the app renders them all with one screen.

    The catalog is GENERATED, never hand-edited. A hand-written catalog drifts from
    the repo silently; a derived one cannot. Re-run this after changing the skills tree.

    Composition is by id, not by nesting: a reference used by two skills appears once
    in `dsos` and twice in the parents' `resources` lists. That is what makes reuse
    real rather than a copy per parent.
#>
[CmdletBinding()]
param(
    [string] $SkillsRoot = 'T:\pmcro-marketplace',
    [string] $OutFile = 'T:\pmcro-marketplace\ProjectName.App\Resources\Raw\DsoCatalog.json'
)

$ErrorActionPreference = 'Stop'

$pluginsRoot = Join-Path $SkillsRoot 'plugins'
if (-not (Test-Path $pluginsRoot)) { throw "No plugins directory under $SkillsRoot" }

$dsos = [ordered]@{}

function Slug([string] $value) {
    ($value -replace '[^A-Za-z0-9]+', '-').Trim('-').ToLowerInvariant()
}

function Add-Dso($id, $kind, $name, $summary, $path) {
    if (-not $dsos.Contains($id)) {
        $dsos[$id] = [ordered]@{
            id        = $id
            kind      = $kind
            name      = $name
            summary   = $summary
            path      = $path
            landing   = ''
            screen    = ''
            resources = New-Object System.Collections.ArrayList
        }
    }
    return $dsos[$id]
}

function Link($parent, $childId) {
    if ($parent -and -not $parent.resources.Contains($childId)) {
        [void]$parent.resources.Add($childId)
    }
}

# A named front-matter field from the head of a SKILL.md, when present.
function Read-FrontMatter([string] $file, [string] $field) {
    if (-not (Test-Path $file)) { return '' }
    # -Encoding UTF8 matters: without it Windows PowerShell reads the files as ANSI
    # and every em-dash in a description lands in the catalog as mojibake.
    foreach ($line in Get-Content $file -TotalCount 16 -Encoding UTF8) {
        if ($line -match "^$field`:\s*(.+)$") { return $Matches[1].Trim() }
    }
    return ''
}

function Add-Folder($parent, [string] $folder, [string] $kind, [string] $pattern) {
    if (-not (Test-Path $folder)) { return }
    foreach ($file in Get-ChildItem $folder -File -Filter $pattern -Recurse) {
        $rel = $file.FullName.Substring($SkillsRoot.Length).TrimStart('\', '/') -replace '\\', '/'
        $id = Slug $rel
        $d = Add-Dso $id $kind $file.BaseName '' $rel
        Link $parent $id
    }
}

foreach ($pluginDir in Get-ChildItem $pluginsRoot -Directory) {
    $pluginRel = "plugins/$($pluginDir.Name)"
    $pluginId = Slug $pluginRel

    $manifest = @(
        (Join-Path $pluginDir.FullName 'plugin.json'),
        (Join-Path $pluginDir.FullName '.claude-plugin\plugin.json')
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1

    $summary = ''
    if ($manifest) {
        try { $summary = (Get-Content $manifest -Raw | ConvertFrom-Json).description } catch { $summary = '' }
    }

    $plugin = Add-Dso $pluginId 'Plugin' $pluginDir.Name $summary $pluginRel

    # Skills - each is its own DSO whose landing document is its SKILL.md.
    $skillsDir = Join-Path $pluginDir.FullName 'skills'
    if (Test-Path $skillsDir) {
        foreach ($skillDir in Get-ChildItem $skillsDir -Directory) {
            $skillRel = "$pluginRel/skills/$($skillDir.Name)"
            $skillMd = Join-Path $skillDir.FullName 'SKILL.md'
            $skill = Add-Dso (Slug $skillRel) 'Skill' $skillDir.Name (Read-FrontMatter $skillMd 'description') $skillRel
            $skill.landing = if (Test-Path $skillMd) { "$skillRel/SKILL.md" } else { '' }

            # A skill may declare which screen shape it renders as. Undeclared skills
            # fall back by kind in the app rather than being forced to annotate first.
            $skill.screen = Read-FrontMatter $skillMd 'screen'
            Link $plugin $skill.id

            Add-Folder $skill (Join-Path $skillDir.FullName 'references') 'Reference' '*.md'
            Add-Folder $skill (Join-Path $skillDir.FullName 'scripts')    'Script'    '*'
            Add-Folder $skill (Join-Path $skillDir.FullName 'assets')     'Asset'     '*'
        }
    }

    # Plugin-level resources, shared by every skill in the plugin.
    Add-Folder $plugin (Join-Path $pluginDir.FullName 'references') 'Reference' '*.md'
    Add-Folder $plugin (Join-Path $pluginDir.FullName 'assets')     'Asset'     '*'
    Add-Folder $plugin (Join-Path $pluginDir.FullName 'agents')     'Agent'     '*.agent.md'
    Add-Folder $plugin (Join-Path $pluginDir.FullName 'hooks')      'Tool'      '*'
}

# Repo-level governance documents are Reference DSOs too - they are what the
# Checker verifies against, so they belong in the same graph.
$govId = Slug 'pmcro-governance'
$gov = Add-Dso $govId 'Plugin' 'pmcro governance' 'Laws, earned constraints and conventions the Checker verifies against.' 'docs/governance'
Add-Folder $gov (Join-Path $SkillsRoot 'docs\governance') 'Reference' '*.md'

$catalog = [ordered]@{
    generated = (Get-Date).ToString('o')
    source    = $SkillsRoot
    roots     = @($dsos.Values | Where-Object { $_.kind -eq 'Plugin' } | ForEach-Object { $_.id })
    dsos      = @($dsos.Values)
}

$dir = Split-Path $OutFile -Parent
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }

# UTF-8 without BOM - written with .NET rather than Set-Content, which emits a BOM.
$json = $catalog | ConvertTo-Json -Depth 8
[System.IO.File]::WriteAllText($OutFile, $json, (New-Object System.Text.UTF8Encoding $false))

"wrote $OutFile"
"  roots: $($catalog.roots.Count)   dsos: $($catalog.dsos.Count)"
$dsos.Values | Group-Object kind | Sort-Object Count -Descending | ForEach-Object { "  {0,-10} {1}" -f $_.Name, $_.Count }
