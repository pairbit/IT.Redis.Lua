if redis.call('exists', KEYS[1]) == 1 then
	return redis.call('hsetnx', KEYS[1], ARGV[1], ARGV[2])
else
	return -1
end