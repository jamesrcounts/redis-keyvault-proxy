# KeyVault as Credential Proxy for Redis

## Setup

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

## Result

When the base infrastructure layer deploys, Redis and Key Vault will both be created. Terraform will inject the Redis connection strings and access keys into KeyVault.  If these values ever change, Terraform will update KeyVault next time it runs.

The CacheWorker app starts up and pulls the connection strings from KeyVault.  It uses managed identity when running as a container instance, otherwise uses user identity if debugging locally.  The worker connects to Redis using both connection strings and performs the same basic operations over and over in a loop. The worker continues to work even if one access key is rotated.  It seems to stall out if both access keys are rotated.  To account for key rotation, the worker should be updated to refresh the connection strings for KeyVault from time to time, and restabilish the Redis connection after the refresh.

The managed identity on the worker has no rights to Redis.  It only has the connection strings provided by KeyVault and cannot interact with the Redis control plane.