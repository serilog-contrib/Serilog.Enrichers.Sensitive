param([string]$currentVersion = $(throw "currentVersion is required"))

$lines = get-content Changelog.md

$started = $false
$output = @()

for($index = 0; $index -lt $lines.Length; $index++)
{
    $line = $lines[$index].Trim()
    if($line -eq "## $currentVersion")
    {
        $started = $true
    }
    elseif($started -and $line.StartsWith("## "))
    {
        break
    }
    elseif($started) 
    {
        $output += $line
    }
}

$output > version-changelog.md