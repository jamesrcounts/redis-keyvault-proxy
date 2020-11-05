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
    cpu    = "0.5"
    image  = "acrrediskeyvaultproxy.azurecr.io/cacheworker:latest"
    memory = "1.5"
    name   = "cache-worker"

    environment_variables = {}

    ports {
      port     = 22
      protocol = "TCP"
    }
  }

  diagnostics {
    log_analytics {
      // log_type      = "ContainerInstanceLogs"
      metadata      = {}
      workspace_id  = data.azurerm_log_analytics_workspace.insights.workspace_id
      workspace_key = data.azurerm_log_analytics_workspace.insights.primary_shared_key
    }
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.cache_worker.id]
  }

  image_registry_credential {
    password = random_password.password.result
    server   = data.azurerm_container_registry.acr.login_server
    username = azuread_service_principal.cache_worker_principal.application_id
  }
}
