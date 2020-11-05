resource "azuread_application" "cache_worker" {
  name = "cache-worker"
}

resource "azuread_service_principal" "cache_worker_principal" {
  application_id = azuread_application.cache_worker.application_id
}

resource "azuread_service_principal_password" "cache_worker_principal_password" {
  service_principal_id = azuread_service_principal.cache_worker_principal.id
  description          = "cache-worker password"
  value                = random_password.password.result
  end_date_relative    = "240h"
}

resource "random_password" "password" {
  length           = 16
  special          = true
  override_special = "_%@"
}