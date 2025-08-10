import moment from 'moment';

function formatDate(date: string) {
  return moment(date).format('MMMM DD, YYYY');
}

export default formatDate;