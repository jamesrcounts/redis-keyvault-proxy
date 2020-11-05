locals {
  project = "redis-keyvault-proxy"

  tags = {
    Environment = "Test"
  }
}

resource "random_pet" "fido" {}