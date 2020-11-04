resource "azurerm_container_registry" "acr" {
  admin_enabled       = false
  location            = azurerm_resource_group.primary.location
  name                = replace("acr-${local.project}", "-", "")
  resource_group_name = azurerm_resource_group.primary.name
  sku                 = "Basic"
  tags                = local.tags
}

