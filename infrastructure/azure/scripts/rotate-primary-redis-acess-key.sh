#!/usr/bin/env bash
set -euox pipefail

az redis regenerate-keys --key-type Primary \
                         --name rc-sharp-satyr \
                         --resource-group rg-redis-keyvault-proxy