# Testing Docker Containers
This document contains commands to test the docker containers are running correctly:

## Redis
```bash
redis-cli -h 127.0.0.1 -p 6380 -a <REDIS PASSWORD>

127.0.0.1:6380> KEYS "*"
```

## Localstack

You don't need real credentials, but AWS CLI still expects a profile:

```powershell
aws configure --profile localstack-test
```
Enter dummy values:
```powershell
AWS Access Key ID [None]: test
AWS Secret Access Key [None]: test
Default region name [None]: us-east-1
Default output format [None]: json
```

### Test S3 Commands
Run these commands to test the LocalStack S3 service:

Create a bucket
```powershell
aws --endpoint-url=http://localhost:4566 s3 mb s3://my-bucket --profile localstack-test
```
Upload a file
```powershell
echo "hello from localstack" > test.txt
aws --endpoint-url=http://localhost:4566 s3 cp test.txt s3://my-bucket/test.txt --profile localstack-test
```
List the bucket's contents
```powershell
aws --endpoint-url=http://localhost:4566 s3 ls s3://my-bucket --profile localstack-test
```
Download the file back
```powershell
aws --endpoint-url=http://localhost:4566 s3 cp s3://my-bucket/test.txt downloaded.txt --profile localstack-test
cat downloaded.txt
```
You should see: hello from localstack

### Test opensearch Commands
Create a domain name:
```powershell
aws --endpoint-url=http://localhost:4566 opensearch create-domain --domain-name app-search-domain --profile localstack-test
```

To view the created domain, you can use the following command:-
```powershell
aws --endpoint-url=http://localhost:4566 opensearch describe-domain --domain-name app-search-domain --profile localstack-test
```