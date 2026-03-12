import { useEffect, useState } from 'react';
import { Card, Row, Col, Statistic, Table } from 'antd';
import { useAuthStore } from '../stores/authStore';
import { dashboardApi, type LiveSummaryDto, type UserPerformanceDto, type TopProductDto } from '../api/client';

export function DashboardPage() {
  const user = useAuthStore((s) => s.user);
  const [liveSummary, setLiveSummary] = useState<LiveSummaryDto[]>([]);
  const [userPerf, setUserPerf] = useState<UserPerformanceDto[]>([]);
  const [topProducts, setTopProducts] = useState<TopProductDto[]>([]);

  useEffect(() => {
    dashboardApi.liveSummary().then(({ data }) => data.success && data.data && setLiveSummary(data.data));
    dashboardApi.userPerformance().then(({ data }) => data.success && data.data && setUserPerf(data.data));
    dashboardApi.topProducts({ top: 5 }).then(({ data }) => data.success && data.data && setTopProducts(data.data));
  }, []);

  const totalComments = liveSummary.reduce((s, x) => s + x.newCount, 0);
  const totalOrders = liveSummary.reduce((s, x) => s + x.orderCount, 0);
  const totalRevenue = liveSummary.reduce((s, x) => s + x.estimatedRevenue, 0);

  return (
    <div>
      <h2>Dashboard</h2>
      <p>Chào {user?.displayName || user?.username}, cửa hàng: {user?.shopName}</p>
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card><Statistic title="Phiên livestream (hôm nay)" value={liveSummary.length} /></Card>
        </Col>
        <Col span={6}>
          <Card><Statistic title="Comment chờ xử lý" value={totalComments} /></Card>
        </Col>
        <Col span={6}>
          <Card><Statistic title="Đơn hàng" value={totalOrders} /></Card>
        </Col>
        <Col span={6}>
          <Card><Statistic title="Doanh thu ước tính" value={totalRevenue} precision={0} suffix="₫" /></Card>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={12}>
          <Card title="Tổng quan theo phiên" size="small">
            <Table size="small" rowKey="liveSessionId" dataSource={liveSummary} pagination={false}
              columns={[
                { title: 'Phiên', dataIndex: 'liveSessionName', key: 'liveSessionName' },
                { title: 'Comment', dataIndex: 'commentCount', key: 'commentCount', width: 80 },
                { title: 'Mới', dataIndex: 'newCount', key: 'newCount', width: 60 },
                { title: 'Đơn', dataIndex: 'orderCount', key: 'orderCount', width: 60 },
                { title: 'Doanh thu', dataIndex: 'estimatedRevenue', key: 'estimatedRevenue', render: (v: number) => v?.toLocaleString('vi-VN') + ' ₫' },
              ]} />
          </Card>
        </Col>
        <Col span={12}>
          <Card title="Hiệu suất nhân viên" size="small">
            <Table size="small" rowKey="userId" dataSource={userPerf} pagination={false}
              columns={[
                { title: 'Nhân viên', dataIndex: 'userName', key: 'userName' },
                { title: 'Comment xử lý', dataIndex: 'commentsHandled', key: 'commentsHandled', width: 100 },
                { title: 'Đơn tạo', dataIndex: 'ordersCreated', key: 'ordersCreated', width: 80 },
                { title: 'Doanh thu', dataIndex: 'orderRevenue', key: 'orderRevenue', render: (v: number) => v?.toLocaleString('vi-VN') + ' ₫' },
              ]} />
          </Card>
        </Col>
      </Row>
      <Card title="Top sản phẩm" size="small" style={{ marginTop: 16 }}>
        <Table size="small" rowKey="productId" dataSource={topProducts} pagination={false}
          columns={[
            { title: 'Sản phẩm', dataIndex: 'productName', key: 'productName' },
            { title: 'SL bán', dataIndex: 'quantitySold', key: 'quantitySold', width: 80 },
            { title: 'Doanh thu', dataIndex: 'revenue', key: 'revenue', render: (v: number) => v?.toLocaleString('vi-VN') + ' ₫' },
          ]} />
      </Card>
    </div>
  );
}
