import json
import boto3

region='sa-east-1'

ec2 = boto3.resource('ec2', region_name=region)

def lambda_handler(event, context):
    instances = ec2.instances.filter(Filters=[{
        'Name': 'tag:Turma','Values': ['6NETT']
    }])
    for instance in instances:
        id=instance.id
        ec2.instances.filter(InstanceIds=[id]).stop()
        print('started instances: ' +str(instance.id))
    return "success"