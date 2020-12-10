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