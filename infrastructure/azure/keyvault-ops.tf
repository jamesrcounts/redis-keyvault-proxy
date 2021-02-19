resource "azurerm_key_vault" "ops" {
  enable_rbac_authorization       = true
  enabled_for_deployment          = false
  enabled_for_disk_encryption     = false
  enabled_for_template_deployment = false
  location                        = azurerm_resource_group.primary.location
  name                            = replace("kv-ops-${local.project}", "-", "")
  resource_group_name             = azurerm_resource_group.primary.name
  sku_name                        = "standard"
  soft_delete_retention_days      = 30
  tenant_id                       = data.azurerm_client_config.current.tenant_id
  tags                            = local.tags

  contact {
    email = "joe@olive-steel.com"
    name  = "Joe Secrets"
    phone = "(555) 555-5555"
  }
}

resource "azurerm_key_vault_secret" "cache_worker_principal_id" {
  name         = "cache-worker-principal-id"
  value        = azuread_service_principal.cache_worker_principal.application_id
  key_vault_id = azurerm_key_vault.ops.id
  tags         = local.tags
}

resource "azurerm_key_vault_secret" "cache_worker_principal_password" {
  name         = "cache-worker-principal-password"
  value        = random_password.cache_worker_password.result
  key_vault_id = azurerm_key_vault.ops.id
  tags         = local.tags
}