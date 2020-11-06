#!/usr/bin/env bash
set -euox pipefail

az redis regenerate-keys --key-type Secondary \
                         --name rc-valid-hookworm \
                         --resource-group rg-redis-keyvault-proxy