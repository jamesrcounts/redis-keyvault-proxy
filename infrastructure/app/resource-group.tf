data "azurerm_resource_group" "primary" {
  name = "rg-${local.project}"
}

resource "azurerm_role_assignment" "acr_pull" {
  principal_id         = azuread_service_principal.cache_worker_principal.id
  role_definition_name = "AcrPull"
  scope                = data.azurerm_resource_group.primary.id
}

resource "azurerm_role_assignment" "key_vault_secret_reader" {
  principal_id         = azurerm_user_assigned_identity.cache_worker.principal_id
  role_definition_name = "Key Vault Secrets User (preview)"
  scope                = data.azurerm_resource_group.primary.id
}