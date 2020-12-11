default: apply

clean: connect
	terraform destroy -auto-approve

connect:
	terraform init -get-plugins=false -get=false

init:
	terraform init -upgrade

format:
	terraform fmt -recursive

validate:
	terraform validate

apply: connect format validate
	terraform apply

rotate_primary:
	ID=$$(terraform output -json redis_cache_name | jq -r); \
	az redis regenerate-keys --key-type Primary \
		--name $${ID} \
		--resource-group rg-redis-keyvault-proxy

rotate_secondary:
	ID=$$(terraform output -json redis_cache_name | jq -r); \
	az redis regenerate-keys --key-type Secondary \
		--name $${ID} \
		--resource-group rg-redis-keyvault-proxy