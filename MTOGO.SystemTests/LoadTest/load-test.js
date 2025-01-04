import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '5m', target: 200000 },  
        { duration: '10m', target: 200000 },
        { duration: '5m', target: 0 },      
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],  
    },
};

const BASE_URL = 'http://host.docker.internal:5004'; 
const ENDPOINT = `${BASE_URL}/api/restaurant/AllRestaurants`;

export default function () {
    const res = http.get(ENDPOINT);

    check(res, {
        'status is 200': (r) => r.status === 200,
        'response time < 500ms': (r) => r.timings.duration < 500,
    });

    sleep(1);
}

