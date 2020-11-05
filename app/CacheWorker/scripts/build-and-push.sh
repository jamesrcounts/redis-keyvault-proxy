#!/usr/bin/env bash
set -euox pipefail

pushd ..
az acr build -t cacheworker:latest -t cacheworker:{{.Run.ID}} -r acrrediskeyvaultproxy -f Dockerfile .
popd 