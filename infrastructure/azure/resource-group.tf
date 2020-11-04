resource "azurerm_resource_group" "primary" {
  name     = "rg-${local.project}"
  location = "centralus"
  tags     = local.tags
}

data "azurerm_client_config" "current" {}

resource "azurerm_role_assignment" "contact_manager" {
  principal_id         = data.azurerm_client_config.current.object_id
  role_definition_name = "Key Vault Administrator (preview)"
  scope                = azurerm_resource_group.primary.id
}