resource "azurerm_resource_group" "primary" {
  name     = "rg-${local.project}"
  location = "centralus"
  tags     = local.tags
}
