import * as k8s from '@kubernetes/client-node';

// Carregando a configuração do Kubernetes
const kc = new k8s.KubeConfig();
kc.loadFromDefault();

// Criando um objeto de configuração do cliente
const k8sApi = kc.makeApiClient(k8s.CoreV1Api);

// Definindo o manifesto YAML para o pod
const podManifest = `
apiVersion: v1
kind: Pod
metadata:
 name: c-shima
spec:
 containers:
 - name: pod-shima
   image: tadrianonet/shima-image:v1
`;

// Carregando o manifesto YAML para um objeto JSON
const podManifestJson = JSON.parse(JSON.stringify(k8s.loadYaml(podManifest)));

// Criando o objeto do pod
k8sApi.createNamespacedPod({ namespace: 'default', body: podManifestJson })
 .then((response) => {
   console.log('Pod criado com sucesso:', response.metadata.name);
 })
 .catch((error) => {
   console.error('Erro ao criar o pod:', error);
 });
