pushd Quasar.AspNetCore

pushd app

Start-Process "npx" -ArgumentList "quasar dev" -NoNewWindow
popd

Start-Process "dotnet" -ArgumentList "run" -NoNewWindow
popd
