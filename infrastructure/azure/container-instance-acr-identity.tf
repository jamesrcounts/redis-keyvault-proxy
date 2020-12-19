# To pull from ACR, the ACI instance needs a Service Principal.  ACI only makes 
# managed identity available inside the container, not to the PaaS pulling the 
# image: https://docs.microsoft.com/en-us/azure/container-instances/container-instances-managed-identity#limitations

resource "azuread_application" "cache_worker" {
  name = "ea-cache-worker"
}

resource "azuread_service_principal" "cache_worker_principal" {
  application_id = azuread_application.cache_worker.application_id
}

resource "azuread_service_principal_password" "cache_worker_principal_password" {
  service_principal_id = azuread_service_principal.cache_worker_principal.id
  description          = "cache-worker password"
  value                = random_password.cache_worker_password.result
  end_date_relative    = "240h"
}

resource "random_password" "cache_worker_password" {
  length           = 16
  special          = true
  override_special = "_%@"
}