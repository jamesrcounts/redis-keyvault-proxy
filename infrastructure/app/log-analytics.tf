data "azurerm_log_analytics_workspace" "insights" {
  name                = "la-${local.project}"
  resource_group_name = data.azurerm_resource_group.primary.name
}
