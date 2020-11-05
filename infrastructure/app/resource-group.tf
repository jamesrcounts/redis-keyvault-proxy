data "azurerm_resource_group" "primary" {
  name = "rg-${local.project}"
}
