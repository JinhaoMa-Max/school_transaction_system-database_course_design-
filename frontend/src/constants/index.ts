export const conditionMap: Record<string, string> = {
  new: '全新',
  like_new: '几乎全新',
  slight_use: '轻微使用',
  obvious_trace: '明显痕迹'
}

export const goodsStatusMap: Record<string, { text: string; color: string }> = {
  pending: { text: '待审核', color: 'orange' },
  approved: { text: '已上架', color: 'green' },
  rejected: { text: '已驳回', color: 'red' },
  locked: { text: '已锁定', color: 'gold' },
  sold: { text: '已售出', color: 'cyan' },
  offline: { text: '已下架', color: 'gray' }
}

export const orderStatusMap: Record<string, { text: string; color: string }> = {
  pending_meet: { text: '待面交', color: 'orange' },
  in_meet: { text: '面交中', color: 'blue' },
  completed: { text: '已完成', color: 'green' },
  cancelled: { text: '已取消', color: 'gray' }
}

export const appointmentStatusMap: Record<string, { text: string; color: string }> = {
  pending: { text: '待确认', color: 'orange' },
  confirmed: { text: '已确认', color: 'blue' },
  completed: { text: '已完成', color: 'green' },
  cancelled: { text: '已取消', color: 'gray' }
}

export const bargainStatusMap: Record<string, { text: string; color: string }> = {
  active: { text: '进行中', color: 'blue' },
  accepted: { text: '已达成', color: 'green' },
  rejected: { text: '已拒绝', color: 'red' },
  closed: { text: '已关闭', color: 'gray' }
}

export const sellerResultMap: Record<string, { text: string; color: string }> = {
  pending: { text: '待处理', color: 'orange' },
  accepted: { text: '已接受', color: 'green' },
  rejected: { text: '已拒绝', color: 'red' },
  countered: { text: '已还价', color: 'blue' }
}