import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = 'http://localhost:5196';

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '1m', target: 10 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<800'],
    checks: ['rate>0.95'],
  },
};

export default function () {
  const resAcessos = http.get(`${BASE_URL}/api/Acessos`);

  check(resAcessos, {
    'GET /api/Acessos retornou 200': (r) => r.status === 200,
  });

  sleep(1);
}