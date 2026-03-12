import { Card, Row, Col, Statistic } from 'antd';
import { useAuthStore } from '../stores/authStore';

export function DashboardPage() {
  const user = useAuthStore((s) => s.user);

  return (
    <div>
      <h2>Dashboard</h2>
      <p>Chào {user?.displayName || user?.username}, cửa hàng: {user?.shopName}</p>
      <Row gutter={16}>
        <Col span={6}>
          <Card>
            <Statistic title="Phiên livestream hôm nay" value={0} />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic title="Comment chờ xử lý" value={0} />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic title="Đơn hàng hôm nay" value={0} />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic title="Doanh thu ước tính" value={0} suffix="₫" />
          </Card>
        </Col>
      </Row>
    </div>
  );
}
