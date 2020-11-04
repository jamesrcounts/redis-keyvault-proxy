# NOTE: the Name used for Redis needs to be globally unique
resource "azurerm_redis_cache" "cache" {
  capacity            = 0
  enable_non_ssl_port = false
  family              = "C"
  location            = azurerm_resource_group.primary.location
  minimum_tls_version = "1.2"
  name                = "rc-${random_pet.fido.id}"
  resource_group_name = azurerm_resource_group.primary.name
  sku_name            = "Basic"
  tags                = local.tags

  redis_configuration {}

  patch_schedule {
    day_of_week    = "Sunday"
    start_hour_utc = 2
  }
}