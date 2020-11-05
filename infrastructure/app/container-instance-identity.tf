resource "azurerm_user_assigned_identity" "cache_worker" {
  location            = data.azurerm_resource_group.primary.location
  name                = "cache-worker"
  resource_group_name = data.azurerm_resource_group.primary.name
}