if ($env:APPVEYOR_REPO_BRANCH -ne "master")
{
 Write-Host 'Skip "HockeyApp" deployment as no branches matched (build branch is "'"$env:APPVEYOR_REPO_BRANCH"'", deploy on branches "master")'
 exit 0
}

Get-ChildItem .\Setup\Out\*.msi | ForEach-Object {
  $msi=$_.FullName

  & C:\ProgramData\Chocolatey\bin\curl -sLk `
    -F "status=2" `
    -F "notify=1" `
    -F "commit_sha=$env:APPVEYOR_REPO_COMMIT" `
    -F "build_server_url=https://ci.appveyor.com/project/$APPVEYOR_ACCOUNT_NAME/$APPVEYOR_PROJECT_NAME/build/$env:APPVEYOR_BUILD_VERSION" `
    -F "repository_url=http://github.com/$APPVEYOR_REPO_NAME" `
    -F "release_type=2" `
    -F "notes=$APPVEYOR_REPO_COMMIT_MESSAGE" `
    -F "notes_type=1" `
    -F "mandatory=0" `
    -F "teams=$env:HOCKEYAPP_TEAM_IDS" `
    -F "ipa=@$msi" `
    -H "X-HockeyAppToken: $env:HOCKEYAPP_TOKEN" `
    https://rink.hockeyapp.net/api/2/apps/$env:HOCKEYAPP_ID/app_versions/upload
}

