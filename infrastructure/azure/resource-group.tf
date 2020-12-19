resource "azurerm_resource_group" "primary" {
  name     = "rg-${local.project}"
  location = "centralus"
  tags     = local.tags
}

resource "azurerm_role_assignment" "acr_pull" {
  principal_id         = azuread_service_principal.cache_worker_principal.id
  role_definition_name = "AcrPull"
  scope                = azurerm_resource_group.primary.id
}

