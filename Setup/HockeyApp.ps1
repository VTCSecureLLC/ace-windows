Get-ChildItem C:\Setup\Out\*.msi | ForEach-Object {
  $msi=$_.Name

  echo C:\ProgramData\Chocolatey\bin\curl -k `
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
    -F "ipa=C:\Setup\Out\$msi" `
    -H "X-HockeyAppToken: REDACTED" `
    https://rink.hockeyapp.net/api/2/apps/REDACTED/app_versions/upload

  & C:\ProgramData\Chocolatey\bin\curl -k `
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
    -F "ipa=C:\Setup\Out\$msi" `
    -H "X-HockeyAppToken: $env:HOCKEYAPP_TOKEN" `
    https://rink.hockeyapp.net/api/2/apps/$env:HOCKEYAPP_ID/app_versions/upload
}

