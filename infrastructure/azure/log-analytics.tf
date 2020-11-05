resource "azurerm_log_analytics_workspace" "insights" {
  name                = "la-${local.project}"
  location            = azurerm_resource_group.primary.location
  resource_group_name = azurerm_resource_group.primary.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = local.tags
}
