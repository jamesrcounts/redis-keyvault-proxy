#!/usr/bin/env bash
set -euox pipefail

az redis regenerate-keys --key-type Primary \
                         --name rc-flowing-shad \
                         --resource-group rg-redis-keyvault-proxy