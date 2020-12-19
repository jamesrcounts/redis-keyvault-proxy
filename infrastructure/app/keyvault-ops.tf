data "azurerm_key_vault" "ops" {
  name                = "kvopsrediskeyvaultproxy"
  resource_group_name = data.azurerm_resource_group.primary.name
}

data "azurerm_key_vault_secret" "principal_id" {
  name         = "cache-worker-principal-id"
  key_vault_id = data.azurerm_key_vault.ops.id
}

data "azurerm_key_vault_secret" "principal_password" {
  name         = "cache-worker-principal-password"
  key_vault_id = data.azurerm_key_vault.ops.id
}