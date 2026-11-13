import * as k8s from '@kubernetes/client-node';

const kc = new k8s.KubeConfig();
kc.loadFromDefault();

const k8sApi = kc.makeApiClient(k8s.CoreV1Api);

k8sApi.readNamespacedPod({ name: 'apicontador-node', namespace: 'default' }).then((pod) => {
    console.log(pod);
}).catch((err) => {
    console.log(err);
});
