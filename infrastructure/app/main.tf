locals {
  project = "redis-keyvault-proxy"

  tags = {
    project = local.project
  }
}

resource "random_pet" "fido" {}