# KeyVault as Credential Proxy for Redis

1. Deploy base terraform config in ./infrastructure/azure
   1. ACR
   2. KeyVault
   3. Log Analytics
   4. Redis
2. Push container image by running ./app/CacheWorker/scripts/build-and-push.sh
   1. Depends on the ACR created by the previous step.
3. Deploy application specific terraform config in ./infrastructure/app
   1. Depends on the image deployed to the ACR in the previous step.
   2. Creates
      1. Container Group
      2. Managed Identity - to access KeyVault
      3. Service Principal - to access ACR