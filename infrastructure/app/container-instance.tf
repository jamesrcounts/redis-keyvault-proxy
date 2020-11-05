locals {
  aci_name = "aci-${local.project}"
}

resource "azurerm_container_group" "worker" {
  dns_name_label      = local.aci_name
  ip_address_type     = "public"
  location            = data.azurerm_resource_group.primary.location
  name                = local.aci_name
  os_type             = "Linux"
  resource_group_name = data.azurerm_resource_group.primary.name
  restart_policy      = "Always"
  tags                = local.tags

  container {
    name   = "cache-worker"
    image  = "acrrediskeyvaultproxy/cacheworker:latest"
    cpu    = "0.5"
    memory = "1.5"

    environment_variables = {}
  }

  diagnostics {
    log_analytics {
      log_type      = "ContainerInstanceLogs"
      workspace_id  = data.azurerm_log_analytics_workspace.insights.workspace_id
      workspace_key = data.azurerm_log_analytics_workspace.insights.primary_shared_key
    }
  }

  identity { type = "SystemAssigned" }
}
