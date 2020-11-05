#!/usr/bin/env bash
set -euox pipefail

pushd ..
az acr build -t cacheworker:{{.Run.ID}} -r acrrediskeyvaultproxy -f Dockerfile .
popd 