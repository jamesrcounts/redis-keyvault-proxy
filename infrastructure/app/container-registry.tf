data "azurerm_container_registry" "acr" {
  name                = replace("acr-${local.project}", "-", "")
  resource_group_name = data.azurerm_resource_group.primary.name
}


