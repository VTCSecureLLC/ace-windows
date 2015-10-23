if ($env:APPVEYOR_REPO_BRANCH -ne "master")
{
 Write-Host 'Skip "HockeyApp" deployment as no branches matched (build branch is "'"$env:APPVEYOR_REPO_BRANCH"'", deploy on branches "master")'
}

Get-ChildItem .\Setup\Out\*.msi | ForEach-Object {
  $msi=$_.FullName

  $id=$(& C:\ProgramData\Chocolatey\bin\curl -sLk `
    -F "bundle_version=$env:APPVEYOR_BUILD_NUMBER" `
    -F "bundle_short_version=$env:APPVEYOR_BUILD_VERSION" `
    -F "notes=$APPVEYOR_REPO_COMMIT_MESSAGE" `
    -F "notes_type=1" `
    -F "status=2" `
    -F "teams=$env:HOCKEYAPP_TEAM_IDS" `
    -H "X-HockeyAppToken: $env:HOCKEYAPP_TOKEN" `
    https://rink.hockeyapp.net/api/2/apps/$env:HOCKEYAPP_ID/app_versions/new | jq -r .id)

  Write-Host "Created app version $id for $APPVEYOR_BUILD_VERSION - $APPVEYOR_BUILD_NUMBER"

  & C:\ProgramData\Chocolatey\bin\curl -sLk `
    -F "ipa=@$msi" `
    -F "notes=$APPVEYOR_REPO_COMMIT_MESSAGE" `
    -F "notes_type=1" `
    -F "notify=1" `
    -F "status=2" `
    -F "teams=$env:HOCKEYAPP_TEAM_IDS" `
    -F "mandatory=0" `
    -H "X-HockeyAppToken: $env:HOCKEYAPP_TOKEN" `
    https://rink.hockeyapp.net/api/2/apps/$env:HOCKEYAPP_ID/app_versions/$id
}

