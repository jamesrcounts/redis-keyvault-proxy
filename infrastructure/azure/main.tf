locals {
  project = "redis-keyvault-proxy"

  tags = {
    Environment = "Test"
  }
}

resource "random_pet" "fido" {}

data "azurerm_client_config" "current" {}