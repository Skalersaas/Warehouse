import moment from 'moment';

function formatDate(date: string) {
  return moment(date).format('MMMM DD, YYYY');
}

function toLocalISOString(date: Date | null) {
  if (!date) return "";
  const pad = (num: number) => String(num).padStart(2, '0');
    return `${date?.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}.${date.getMilliseconds()}Z`;
}

export {formatDate, toLocalISOString};