#!/usr/bin/env bash
set -euox pipefail

az redis regenerate-keys --key-type Primary \
                         --name rc-meet-buzzard \
                         --resource-group rg-redis-keyvault-proxy