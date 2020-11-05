data "azurerm_resource_group" "primary" {
  name = "rg-${local.project}"
}

resource "azurerm_role_assignment" "acr_pull" {
  principal_id         = azuread_service_principal.cache_worker_principal.id
  role_definition_name = "AcrPull"
  scope                = data.azurerm_resource_group.primary.id
}

// resource "azurerm_role_assignment" "key_vault_secret_reader" {
//   principal_id         = "b55f6ee9-3a4b-42bf-ad67-d02c4632a010"
//   role_definition_name = "Key Vault Secrets User (preview)"
//   scope                = azurerm_resource_group.primary.id
// }