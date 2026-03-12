import { useEffect, useState } from 'react';
import { Card, Form, Input, InputNumber, Button, Table, Select, message } from 'antd';
import { useSearchParams, useLocation } from 'react-router-dom';
import { productsApi, ordersApi, liveSessionsApi, type ProductListDto, type LiveSessionDto } from '../api/client';

export function QuickOrderPage() {
  const [searchParams] = useSearchParams();
  const location = useLocation();
  const state = (location.state as { receiverName?: string; receiverPhone?: string }) || {};
  const commentId = searchParams.get('commentId') ? Number(searchParams.get('commentId')) : undefined;
  const liveSessionId = searchParams.get('liveSessionId') ? Number(searchParams.get('liveSessionId')) : undefined;
  const [products, setProducts] = useState<ProductListDto[]>([]);
  const [sessions, setSessions] = useState<LiveSessionDto[]>([]);
  const [form] = Form.useForm();
  useEffect(() => {
    if (state.receiverName || state.receiverPhone) form.setFieldsValue({ receiverName: state.receiverName, receiverPhone: state.receiverPhone });
  }, [state.receiverName, state.receiverPhone]);
  const [lines, setLines] = useState<{ productVariantId: number; sku: string; name: string; price: number; quantity: number }[]>([]);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    productsApi.list().then(({ data }) => {
      if (data.success && data.data) setProducts(data.data);
    });
    liveSessionsApi.list().then(({ data }) => {
      if (data.success && data.data) setSessions(data.data);
    });
  }, []);

  const addLine = (variant: { id: number; sku: string; price: number }, productName: string) => {
    if (lines.some((l) => l.productVariantId === variant.id)) return;
    setLines((prev) => [...prev, { productVariantId: variant.id, sku: variant.sku, name: productName, price: variant.price, quantity: 1 }]);
  };

  const removeLine = (productVariantId: number) => {
    setLines((prev) => prev.filter((l) => l.productVariantId !== productVariantId));
  };

  const updateQty = (productVariantId: number, quantity: number) => {
    setLines((prev) => prev.map((l) => (l.productVariantId === productVariantId ? { ...l, quantity } : l)));
  };

  const subTotal = lines.reduce((s, l) => s + l.price * l.quantity, 0);
  const shippingFee = Form.useWatch('shippingFee', form) ?? 0;
  const discount = Form.useWatch('discount', form) ?? 0;
  const total = subTotal + Number(shippingFee) - Number(discount);

  const onFinish = async (v: Record<string, unknown>) => {
    if (lines.length === 0) {
      message.warning('Thêm ít nhất một sản phẩm');
      return;
    }
    setSubmitting(true);
    try {
      const { data } = await ordersApi.create({
        liveSessionId: liveSessionId ?? v.liveSessionId,
        commentId,
        receiverName: v.receiverName as string,
        receiverPhone: v.receiverPhone as string,
        receiverAddress: v.receiverAddress as string,
        note: v.note as string,
        shippingFee: Number(v.shippingFee ?? 0),
        discount: Number(v.discount ?? 0),
        items: lines.map((l) => ({ productVariantId: l.productVariantId, quantity: l.quantity, unitPrice: l.price })),
      });
      if (data.success && data.data) {
        const result = data.data;
        if (result.riskWarning) message.warning(result.riskWarning);
        message.success('Tạo đơn thành công: ' + result.order.orderNo);
        form.resetFields();
        setLines([]);
      } else message.error(data.message || 'Lỗi');
    } catch {
      message.error('Lỗi tạo đơn');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <h2>Quick Order</h2>
      <Card>
        <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ shippingFee: 0, discount: 0 }}>
          {!liveSessionId && (
            <Form.Item name="liveSessionId" label="Phiên livestream">
              <Select placeholder="Chọn phiên" options={sessions.map((s) => ({ value: s.id, label: s.name }))} />
            </Form.Item>
          )}
          <Form.Item name="receiverName" label="Tên người nhận" rules={[{ required: true }]}>
            <Input placeholder="Họ tên" />
          </Form.Item>
          <Form.Item name="receiverPhone" label="Số điện thoại" rules={[{ required: true }]}>
            <Input placeholder="SĐT" />
          </Form.Item>
          <Form.Item name="receiverAddress" label="Địa chỉ">
            <Input.TextArea rows={2} placeholder="Địa chỉ giao hàng" />
          </Form.Item>
          <Form.Item name="note" label="Ghi chú">
            <Input placeholder="Ghi chú đơn hàng" />
          </Form.Item>
          <h4>Thêm sản phẩm</h4>
          <div style={{ marginBottom: 12 }}>
            {products.map((p) =>
              p.variants.map((v) => (
                <Button key={v.id} size="small" style={{ marginRight: 8, marginBottom: 8 }} onClick={() => addLine(v, p.name)}>
                  {p.name} - {v.sku} ({v.price?.toLocaleString('vi-VN')} ₫)
                </Button>
              ))
            )}
          </div>
          <Table
            size="small"
            rowKey="productVariantId"
            dataSource={lines}
            columns={[
              { title: 'Sản phẩm', dataIndex: 'name', key: 'name' },
              { title: 'SKU', dataIndex: 'sku', key: 'sku' },
              { title: 'Đơn giá', dataIndex: 'price', key: 'price', render: (v: number) => v?.toLocaleString('vi-VN') + ' ₫' },
              {
                title: 'SL',
                key: 'quantity',
                width: 100,
                render: (_, r) => (
                  <InputNumber min={1} value={r.quantity} onChange={(v) => updateQty(r.productVariantId, Number(v) || 1)} />
                ),
              },
              { title: 'Thành tiền', key: 'lineTotal', render: (_, r) => (r.price * r.quantity).toLocaleString('vi-VN') + ' ₫' },
              { title: '', key: 'act', width: 60, render: (_, r) => <Button type="link" danger onClick={() => removeLine(r.productVariantId)}>Xóa</Button> },
            ]}
            pagination={false}
          />
          <Form.Item name="shippingFee" label="Phí ship">
            <InputNumber min={0} style={{ width: 160 }} addonAfter="₫" />
          </Form.Item>
          <Form.Item name="discount" label="Giảm giá">
            <InputNumber min={0} style={{ width: 160 }} addonAfter="₫" />
          </Form.Item>
          <p><strong>Tổng cộng: {total.toLocaleString('vi-VN')} ₫</strong></p>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting}>Tạo đơn</Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
}
