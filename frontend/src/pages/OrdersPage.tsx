import { useEffect, useState } from 'react';
import { Card, Table, Select, Input, Drawer, Descriptions, Button, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { ordersApi, liveSessionsApi, type OrderListDto, type OrderDetailDto, type LiveSessionDto } from '../api/client';

const ORDER_STATUS: Record<number, string> = {
  0: 'Nháp',
  1: 'Chờ xác nhận',
  2: 'Đã xác nhận',
  3: 'Đã đóng gói',
  4: 'Đang giao',
  5: 'Đã giao',
  6: 'Đã hủy',
  7: 'Trả hàng',
  8: 'COD thất bại',
};

export function OrdersPage() {
  const [sessions, setSessions] = useState<LiveSessionDto[]>([]);
  const [orders, setOrders] = useState<OrderListDto[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [liveSessionId, setLiveSessionId] = useState<number | undefined>();
  const [status, setStatus] = useState<number | undefined>();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [selected, setSelected] = useState<OrderDetailDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const pageSize = 20;

  const loadSessions = async () => {
    try {
      const { data } = await liveSessionsApi.list();
      if (data.success && data.data) setSessions(data.data);
    } catch {
      setSessions([]);
    }
  };

  const loadOrders = async () => {
    setLoading(true);
    try {
      const { data } = await ordersApi.list({
        liveSessionId,
        status,
        search: search || undefined,
        page,
        pageSize,
      });
      if (data.success && data.data) {
        setOrders(data.data.items);
        setTotal(data.data.totalCount);
      } else {
        setOrders([]);
        setTotal(0);
      }
    } catch {
      setOrders([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSessions();
  }, []);

  useEffect(() => {
    loadOrders();
  }, [liveSessionId, status, page, search]);

  const openDetail = async (id: number) => {
    setDetailLoading(true);
    try {
      const { data } = await ordersApi.getById(id);
      if (data.success && data.data) setSelected(data.data);
      else setSelected(null);
    } catch {
      setSelected(null);
    } finally {
      setDetailLoading(false);
    }
  };

  const columns: ColumnsType<OrderListDto> = [
    { title: 'Mã đơn', dataIndex: 'orderNo', key: 'orderNo', width: 180 },
    { title: 'Người nhận', dataIndex: 'receiverName', key: 'receiverName', width: 120 },
    { title: 'SĐT', dataIndex: 'receiverPhone', key: 'receiverPhone', width: 120 },
    { title: 'Tổng tiền', dataIndex: 'totalAmount', key: 'totalAmount', width: 120, render: (v: number) => v?.toLocaleString('vi-VN') + ' ₫' },
    { title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 120, render: (s: number) => ORDER_STATUS[s] ?? s },
    { title: 'Ngày tạo', dataIndex: 'createdAtUtc', key: 'createdAtUtc', width: 160, render: (v: string) => new Date(v).toLocaleString() },
  ];

  return (
    <div>
      <h2>Đơn hàng</h2>
      <Card>
        <div style={{ marginBottom: 16, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <Input.Search placeholder="Mã đơn, SĐT, tên..." allowClear onSearch={setSearch} style={{ width: 220 }} />
          <Select
            placeholder="Phiên livestream"
            allowClear
            style={{ width: 200 }}
            value={liveSessionId}
            onChange={setLiveSessionId}
            options={[{ value: undefined, label: 'Tất cả' }, ...sessions.map((s) => ({ value: s.id, label: s.name }))]}
          />
          <Select
            placeholder="Trạng thái"
            allowClear
            style={{ width: 140 }}
            value={status}
            onChange={setStatus}
            options={[{ value: undefined, label: 'Tất cả' }, ...Object.entries(ORDER_STATUS).map(([k, v]) => ({ value: Number(k), label: v }))]}
          />
        </div>
        <Table
          rowKey="id"
          columns={columns}
          dataSource={orders}
          loading={loading}
          onRow={(r) => ({ onClick: () => openDetail(r.id), style: { cursor: 'pointer' } })}
          pagination={{ current: page, pageSize, total, showSizeChanger: false, onChange: setPage }}
        />
      </Card>
      <Drawer title="Chi tiết đơn hàng" open={!!selected} onClose={() => setSelected(null)} width={480} loading={detailLoading}>
        {selected && (
          <>
            <Descriptions column={1} size="small">
              <Descriptions.Item label="Mã đơn">{selected.orderNo}</Descriptions.Item>
              <Descriptions.Item label="Trạng thái">{ORDER_STATUS[selected.status] ?? selected.status}</Descriptions.Item>
              <Descriptions.Item label="Người nhận">{selected.receiverName}</Descriptions.Item>
              <Descriptions.Item label="SĐT">{selected.receiverPhone}</Descriptions.Item>
              <Descriptions.Item label="Địa chỉ">{selected.receiverAddress}</Descriptions.Item>
              <Descriptions.Item label="Tạm tính">{selected.subTotal?.toLocaleString('vi-VN')} ₫</Descriptions.Item>
              <Descriptions.Item label="Phí ship">{selected.shippingFee?.toLocaleString('vi-VN')} ₫</Descriptions.Item>
              <Descriptions.Item label="Giảm giá">{selected.discount?.toLocaleString('vi-VN')} ₫</Descriptions.Item>
              <Descriptions.Item label="Tổng cộng">{selected.totalAmount?.toLocaleString('vi-VN')} ₫</Descriptions.Item>
            </Descriptions>
            <h4 style={{ marginTop: 16 }}>Sản phẩm</h4>
            <Table
              size="small"
              rowKey="productVariantId"
              dataSource={selected.items}
              columns={[
                { title: 'SP', dataIndex: 'productName', key: 'productName' },
                { title: 'SL', dataIndex: 'quantity', key: 'quantity', width: 60 },
                { title: 'Đơn giá', dataIndex: 'unitPrice', key: 'unitPrice', render: (v: number) => v?.toLocaleString('vi-VN') },
                { title: 'Thành tiền', dataIndex: 'lineTotal', key: 'lineTotal', render: (v: number) => v?.toLocaleString('vi-VN') + ' ₫' },
              ]}
              pagination={false}
            />
          </>
        )}
      </Drawer>
    </div>
  );
}
